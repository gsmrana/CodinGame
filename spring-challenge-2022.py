import sys
import math
from collections import namedtuple

# entity types
ETYPE_MONSTER = 0
ETYPE_MY_HERO = 1
ETYPE_OP_HERO = 2

# threat_for types
THREAT_FOR_NONE = 0
THREAT_FOR_MINE = 1
THREAT_FOR_OPNT = 2

Entity = namedtuple('Entity', [
    'id', 'type', 'x', 'y', 'shield_life', 'is_controlled', 'health', 'vx', 'vy', 'near_base', 'threat_for'
])

# base_x: The corner of the map representing your base
base_x, base_y = [int(i) for i in input().split()]
heroes_per_player = int(input())  # Always 3

# game loop
while True:
    # health: Your base health
    # mana: Ignore in the first league; Spend ten mana to cast a spell
    my_health, my_mana = [int(j) for j in input().split()]
    enemy_health, enemy_mana = [int(j) for j in input().split()]
    entity_count = int(input())  # Amount of heros and monsters you can see

    monsters = []
    my_heroes = []
    opp_heroes = []
    for i in range(entity_count):
        _id, _type, x, y, shield_life, is_controlled, health, vx, vy, near_base, threat_for = [
            int(j) for j in input().split()]
        entity = Entity(
            _id,            # _id: Unique identifier
            _type,          # _type: 0=monster, 1=your hero, 2=opponent hero
            x, y,           # x,y: Position of this entity
            shield_life,    # shield_life: Ignore for this league; Count down until shield spell fades
            is_controlled,  # is_controlled: Ignore for this league; Equals 1 when this entity is under a control spell
            health,         # health: Remaining health of this monster
            vx, vy,         # vx,vy: Trajectory of this monster
            near_base,      # near_base: 0=monster with no target yet, 1=monster targeting a base
            threat_for      # threat_for: Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
        )

        if _type == ETYPE_MONSTER:
            monsters.append(entity)
        elif _type == ETYPE_MY_HERO:
            my_heroes.append(entity)
        elif _type == ETYPE_OP_HERO:
            opp_heroes.append(entity)
        else:
            assert False

    threats = []
    for m in monsters:
        threat_level = 0
        if m.near_base == 1 and m.threat_for == THREAT_FOR_MINE:
            threat_level = 1000
        elif m.threat_for == THREAT_FOR_MINE:
            threat_level = 500

        dist = math.hypot(base_x - m.x, base_y - m.y)
        threat_level += 500 * (1 / (dist + 1))
        threats.append((threat_level, m))

    threats.sort(reverse=True)

    # debug messages
    # print(f'T[{len(threats)}]:', end=' ', file=sys.stderr, flush=True)
    # for t in threats:
    #     print(f'{t[1].id}', end=',', file=sys.stderr, flush=True)

    for i in range(heroes_per_player):
        # To debug: print("Debug messages...", file=sys.stderr, flush=True)
        target = None
        if threats:
            target = threats[i % len(threats)][1]

        # In the first league: MOVE <x> <y> | WAIT; In later leagues: | SPELL <spellParams>;
        if target:
            print(f'MOVE {target.x} {target.y}')
            # print(f'MOVE {target.x} {target.y} {target.id}')
        else:
            print('WAIT')
