import sys
import math

# w: width of the building.
# h: height of the building.
w, h = [int(i) for i in input().split()]
n = int(input())  # maximum number of turns before game over.
x0, y0 = [int(i) for i in input().split()]  # my initial position.

min_x, min_y = 0, 0
mid_x, mid_y = x0, y0
max_x, max_y = w - 1, h - 1

# game loop
while True:
    # the direction of the bombs from batman's current location
    # (U, UR, R, DR, D, DL, L or UL)
    bomb_dir = input()

    # print(bomb_dir, min_x, mid_x, max_x, min_y,
    #       mid_y, max_y, file=sys.stderr, flush=True)

    if 'L' in bomb_dir:
        max_x = mid_x
        mid_x = int(math.floor((min_x + mid_x) / 2))
    elif 'R' in bomb_dir:
        min_x = mid_x
        mid_x = int(math.ceil((max_x + mid_x) / 2))

    if 'U' in bomb_dir:
        max_y = mid_y
        mid_y = int(math.floor((min_y + mid_y) / 2))
    elif 'D' in bomb_dir:
        min_y = mid_y
        mid_y = int(math.ceil((max_y + mid_y) / 2))

    # To debug: print("Debug messages...", file=sys.stderr, flush=True)
    print(f'{mid_x} {mid_y}')
