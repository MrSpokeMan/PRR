import pickle
import socket
import cv2
import numpy as np
from multiprocessing import Pool


def sobel_filter(img):
    # Sobel filter implementation
    sobel_x = np.array([[-1, 0, 1], [-2, 0, 2], [-1, 0, 1]])
    sobel_y = np.array([[-1, -2, -1], [0, 0, 0], [1, 2, 1]])
    # Apply the filters
    gradient_x = cv2.filter2D(img, -1, sobel_x)
    gradient_y = cv2.filter2D(img, -1, sobel_y)
    # Calculate the gradient magnitude
    gradient_magnitude = np.sqrt(np.square(gradient_x) + np.square(gradient_y))
    return gradient_magnitude


def send_all(sock, data):
    data = pickle.dumps(data)
    sock.sendall(len(data).to_bytes(4, byteorder='big'))
    sock.sendall(data)


def recv_all(sock):
    data = b''
    length = int.from_bytes(sock.recv(4), byteorder='big')
    while len(data) < length:
        packet = sock.recv(4096)
        if not packet:
            break
        data += packet
    return pickle.loads(data)

def split_image(img, n):
    h, w = img.shape[:2]
    h_step = h // n
    w_step = w // n
    parts = []
    for i in range(n):
        for j in range(n):
            part = img[i*h_step:(i+1)*h_step, j*w_step:(j+1)*w_step]
            parts.append(part)
    return parts


def client_main():
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect(('172.20.10.7', 12345))
    fragment = recv_all(client_socket)
    processed_fragment = sobel_filter(fragment)
    send_all(client_socket, processed_fragment)
    client_socket.close()
    print("Client finished")


def server_main(img, n_clients):
    img = cv2.imread(img)
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    fragments = split_image(gray, n_clients)
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('172.20.10.7', 12345))
    server_socket.listen(n_clients)
    print("Server is listening")
    processed_fragments = []
    for i in range(len(fragments)):
        client_socket, client_address = server_socket.accept()
        print(f"Client {i+1} connected: {client_address}")
        send_all(client_socket, fragments[i])
        processed_fragments.append(recv_all(client_socket))
        client_socket.close()
    result_image = np.concatenate(processed_fragments)
    # server_socket.close()
    cv2.imshow('Sobel Filter', result_image)
    cv2.waitKey(0)
    # save the result image
    cv2.imwrite('result.jpg', result_image)
    print("Server finished")


def run_n_clients(n):
    for i in range(n):
        client_main()
    # cv2.destroyAllWindows()


if __name__ == '__main__':
    # use pool to run n_clients and server_main in parallel and show the result
    with Pool(2) as p:
        # p.apply_async(server_main, ('image.png', 4))
        p.apply_async(run_n_clients, (4*4,))
        p.close()
        p.join()