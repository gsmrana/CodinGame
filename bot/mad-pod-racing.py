import sys
import math

# game const
BOARD_SIZE_X = 16000
BOARD_SIZE_Y = 9000
CHECKPOINT_RAD = 600
THRUST_MAX = 100

# game vars
boost_count = 0

# game loop
while True:
    # next_checkpoint_x: x position of the next check point
    # next_checkpoint_y: y position of the next check point
    # next_checkpoint_dist: distance to the next checkpoint
    # next_checkpoint_angle: angle between your pod orientation and the direction of the next checkpoint
    x, y, next_checkpoint_x, next_checkpoint_y, next_checkpoint_dist, next_checkpoint_angle = [
        int(i) for i in input().split()]
    opponent_x, opponent_y = [int(i) for i in input().split()]

    thrust = THRUST_MAX
    angle = abs(next_checkpoint_angle)
    if angle > 65:
        thrust = 0
        #thrust = 10 + int(90 * ((180 - angle) / 180))

    if boost_count <= 0:
        if angle == 0 and next_checkpoint_dist >= 4500:
            thrust = 'BOOST'
            boost_count += 1

    print(f'{next_checkpoint_dist} {next_checkpoint_angle}',
          file=sys.stderr, flush=True)

    # To debug: print("Debug messages...", file=sys.stderr, flush=True)
    print(f'{next_checkpoint_x} {next_checkpoint_y} {thrust}')
