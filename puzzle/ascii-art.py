import sys
import math

SYMBOLS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ?"

l = int(input())
h = int(input())
t = input()
intput_font_rows = []
for i in range(h):
    intput_font_rows.append(input())

font_dict = {}
for i in range(len(SYMBOLS)):
    start = i * l
    end = start + l
    sym_font_rows = []
    for j in range(h):
        sym_font_rows.append(intput_font_rows[j][start:end])
    sym = SYMBOLS[i]
    font_dict[sym] = sym_font_rows

# Write an answer using print
# To debug: print("Debug messages...", file=sys.stderr, flush=True)
for r in range(h):
    for sym in t.upper():
        if sym in font_dict:
            print(font_dict[sym][r], end="")
        else:
            print(font_dict['?'][r], end="")
    print()
