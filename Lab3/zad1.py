from multiprocessing import Pool
import numpy as np
import cv2


def f(x):
    return x*x


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


if __name__ == '__main__':
    with Pool(5) as p:
        print(p.map(f, [1, 2, 3]))

    # open image with pillow and apply Sobel filter on it

    img = cv2.imread('image.png')

    # Convert the image to grayscale
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    # Split image to n^2 parts and use Pool to apply Sobel filter on each part
    n = 4
    h, w = gray.shape
    h_step = h // n
    w_step = w // n
    parts = []
    for i in range(n):
        for j in range(n):
            part = gray[i*h_step:(i+1)*h_step, j*w_step:(j+1)*w_step]
            parts.append(part)

    with Pool(5) as p:
        gradient_magnitudes = p.map(sobel_filter, parts)

    # Combine the gradient magnitudes of the parts
    gradient_magnitude_combined = np.zeros_like(gray)
    for i in range(n):
        for j in range(n):
            gradient_magnitude_combined[i*h_step:(i+1)*h_step, j*w_step:(j+1)*w_step] = gradient_magnitudes[i*n + j]

    # Display the result
    cv2.imshow('Sobel Filter', gradient_magnitude_combined)
    cv2.waitKey(0)