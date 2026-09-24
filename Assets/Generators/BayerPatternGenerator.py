import numpy as np
import cv2

bayer_pattern = [
    [0, 32, 8, 40, 2, 34, 10, 42],
    [48, 16, 56, 24, 50, 18, 58, 26],
    [12, 44, 4, 36, 14, 46, 6, 38],
    [60, 28, 52, 20, 62, 30, 54, 22],
    [3, 35, 11, 43, 1, 33, 9, 41],
    [51, 19, 59, 27, 49, 17, 57, 25], 
    [15, 47, 7, 39, 13, 45, 5, 37],
    [63, 31, 55, 23, 61, 29, 53, 21]
]
arr = (np.array(bayer_pattern) / 64.0 * 255).astype(np.uint8)
print(arr)
cv2.imwrite('output_image.png', arr)

# Show the image
cv2.imshow('Image', arr)
cv2.waitKey(0)
cv2.destroyAllWindows()