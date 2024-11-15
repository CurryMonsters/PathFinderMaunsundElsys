import cv2
import cv2.aruco as aruco
import socket
import numpy as np
import math

# Setup av ArUco dictionary, får inn hvilke Aruco marker("QR" kode) som kan brukes
aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_6X6_250)
parameters = aruco.DetectorParameters()

# Starter opp kamera
# cap = cv2.VideoCapture("http://192.168.137.246:8080/video")
cap = cv2.VideoCapture(1)

# cap.set(cv2.CAP_PROP_FRAME_WIDTH, 725)
# cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 408)
output_resolution = (725, 408)

#Setter opp en UDP socket for å få data inn til Unity
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5051)

# Lager en liste for å lagre verdiene til x,y, rotasjon
last_known_marker_data = [0] * 21

#Verdier for å kalibrere Kamera
camera_matrix = np.array([[1000, 0, 320], [0, 1000, 240], [0, 0, 1]], dtype=np.float32)
dist_coeffs = np.zeros((5, 1))

while True:
    ret, frame = cap.read() #sjekker opp om det er noe innenfor kameras synsvinkel
    if not ret:
        break
    
    # frame = cv2.resize(frame, output_resolution)

    height, width = frame.shape[:2]
    
    # Calculate cropping dimensions to create a centered square frame
    if width > height:
        offset = (width - height) // 2
        frame = frame[:, offset:offset + height]  # Crop to center square
    else:
        offset = (height - width) // 2
        frame = frame[offset:offset + width, :]  # Crop to center square

    # Resize to the specified square resolution
    frame = cv2.resize(frame, (640, 640))

    # Konvertere til svart hvitt
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

    # sjekker om det er noen markers innenfor kameras synsvinkel
    corners, ids, _ = cv2.aruco.detectMarkers(gray, aruco_dict, parameters=parameters)

    # Får kameras høyde og bredde i pixler
    img_height, img_width = frame.shape[:2]

    #Sjekker om hvilkem som helst marker er i bilde
    if ids is not None:
        for i in range(len(ids)):
            # Vil ha spesfikke markerne
            if ids[i] in [1, 2, 3, 4, 5, 6, 7]:
                # Finner sentrum av markeren
                cX = int(np.mean(corners[i][0][:, 0]))  # X-kordinat
                cY = int(np.mean(corners[i][0][:, 1]))  # Y-kordinat

                unity_y = img_height - cY #unity bruker y som z verdi som må konvertere

                # Normaliserer verdier (0-1)
                normalized_x = cX / img_width
                normalized_y = unity_y / img_height

                # Får å finne rotasjon
                rvec, tvec, _ = cv2.aruco.estimatePoseSingleMarkers(corners[i], 0.05, camera_matrix, dist_coeffs)

                # Rotasjons vector
                rotation_matrix, _ = cv2.Rodrigues(rvec)

                yaw_z = math.atan2(rotation_matrix[1, 0], rotation_matrix[0, 0])  # Z rotasjon i radianer

                #lagrer kordinater, og z rotasjonen i en liste
                marker_index = (ids[i][0] - 1) * 3 
                last_known_marker_data[marker_index] = normalized_x
                last_known_marker_data[marker_index + 1] = normalized_y
                last_known_marker_data[marker_index + 2] = yaw_z 

                # egen funksjon for å tegne en sirkel i midten av markeren
                cv2.circle(frame, (cX, cY), 5, (0, 255, 0), -1)

    # Her kommer delen hvor vi sender data til Unity, vi konverterer listen til en string og sender den over UDP.
    sock.sendto(str.encode(str(last_known_marker_data)), serverAddressPort)

    cv2.imshow('frame', frame)

    # Breaker ut av loopen når vi trykker på q
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
