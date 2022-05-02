import sys
import math
from collections import namedtuple
from telnetlib import NOP

# entity types
ETYPE_MONSTER = 0
ETYPE_MY_HERO = 1
ETYPE_OP_HERO = 2

# threat_for types
THREAT_FOR_NONE = 0
THREAT_FOR_MINE = 1
THREAT_FOR_OPNT = 2

# game const
BOARD_SIZE_X = 17630
BOARD_SIZE_Y = 9000

Entity = namedtuple('Entity', [
    'id', 'type', 'x', 'y', 'shield_life', 'is_controlled',
    'health', 'vx', 'vy', 'near_base', 'threat_for'
])

# game vars
game_turn = 0

# base_x: The corner of the map representing your base
mybase_x, mybase_y = [int(i) for i in input().split()]
heroes_per_player = int(input())  # Always 3

opbase_x = BOARD_SIZE_X
opbase_y = BOARD_SIZE_Y
myheros_tent = [
    [5795, 1552],
    [7000, 4800],
    [1552, 5795]
]

if mybase_x != 0:
    opbase_x = 0
    opbase_y = 0
    for i in range(heroes_per_player):
        myheros_tent[i][0] = mybase_x - myheros_tent[i][0]
        myheros_tent[i][1] = mybase_y - myheros_tent[i][1]

# game loop
while True:
    # health: Your base health
    # mana: Ignore in the first league; Spend ten mana to cast a spell
    my_health, my_mana = [int(j) for j in input().split()]
    enemy_health, enemy_mana = [int(j) for j in input().split()]
    entity_count = int(input())  # Amount of heros and monsters you can see

    monsters = []
    my_heroes = []
    op_heroes = []
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
            threat_for     # threat_for: Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
        )

        if _type == ETYPE_MONSTER:
            monsters.append(entity)
        elif _type == ETYPE_MY_HERO:
            my_heroes.append(entity)
        elif _type == ETYPE_OP_HERO:
            op_heroes.append(entity)
        else:
            assert False

     # sort threats by threat level des
    mythreats = []
    opthreats = []
    for monster in monsters:
        threat_level = 0
        if monster.threat_for == THREAT_FOR_OPNT:
            dist_from_opbase = math.hypot(
                opbase_x - monster.x, opbase_y - monster.y)
            threat_level += 500 * (20000 / (dist_from_mybase + 1))
            opthreats.append((threat_level, monster))
        else:
            if monster.threat_for == THREAT_FOR_MINE:
                threat_level += 1000
            dist_from_mybase = math.hypot(
                mybase_x - monster.x, mybase_y - monster.y)
            if dist_from_mybase > 9000:
                continue
            threat_level += 500 * (20000 / (dist_from_mybase + 1))
            threat_level += 100 * (monster.health / 30)
            mythreats.append((threat_level, monster))

    for ophero in op_heroes:
        threat_level = 1000
        dist_from_mybase = math.hypot(
            mybase_x - ophero.x, mybase_y - ophero.y)
        if game_turn > 100:
            threat_level += 2005
        if dist_from_mybase < 1000:
            threat_level += 500 * (20000 / (dist_from_mybase + 1))
        mythreats.append((threat_level, ophero))

    mythreats.sort(reverse=True)
    opthreats.sort(reverse=True)
    print(mythreats, file=sys.stderr, flush=True)

    # find nearest heros
    threat_index = 0
    nearest_threats = [None, None, None]
    for threat in mythreats:
        dist_from_myheros = []
        for i in range(heroes_per_player):
            if nearest_threats[i] != None:
                continue
            if my_heroes[i].is_controlled == 1:
                dist_from_myheros.append((20000, i, None))
            else:
                target = threat[1]
                hero_dist = math.hypot(
                    my_heroes[i].x - target.x, my_heroes[i].y - target.y)
                dist_from_myheros.append((hero_dist, i, threat))
        dist_from_myheros.sort()
        nearest_threats[dist_from_myheros[0][1]] = dist_from_myheros[0][2]
        threat_index += 1
        if threat_index >= heroes_per_player:
            break
    print(nearest_threats, file=sys.stderr, flush=True)

    wind_count = 0
    control_count = 0
    shield_count = 0
    outputs = ['WAIT', 'WAIT', 'WAIT']
    for i in range(heroes_per_player):
        if nearest_threats[i] != None:
            threat_level = nearest_threats[i][0]
            target = nearest_threats[i][1]
            target_dist = math.hypot(
                my_heroes[i].x - target.x, my_heroes[i].y - target.y)
            if target.type == ETYPE_OP_HERO and threat_level > 3000 and control_count <= 0 and my_mana >= 10 and target_dist < 2200:
                outputs[i] = (
                    f'SPELL CONTROL {target.id} {opbase_x} {opbase_y} {target.id}')
                control_count += 1
            elif threat_level > 3000 and wind_count <= 0 and my_mana >= 10 and target_dist < 1280 and target.shield_life == 0:
                outputs[i] = (f'SPELL WIND {opbase_x} {opbase_y} {target.id}')
                wind_count += 1
            else:
                outputs[i] = (f'MOVE {target.x} {target.y} {target.id}')
        else:
            outputs[i] = (
                f'MOVE {myheros_tent[i][0]} {myheros_tent[i][1]} Tent')
    game_turn += 1

    for i in range(heroes_per_player):
        # To debug: print("Debug messages...", file=sys.stderr, flush=True)
        # In the first league: MOVE <x> <y> | WAIT; In later leagues: | SPELL <spellParams>;
        print(outputs[i])
