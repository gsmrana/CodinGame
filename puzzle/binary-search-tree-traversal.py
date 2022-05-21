import sys
import math
import collections


class Node:
    def __init__(self, data):
        self.left = None
        self.right = None
        self.data = data

    def insert(self, data):
        if self.data:
            if data < self.data:
                if self.left:
                    self.left.insert(data)
                else:
                    self.left = Node(data)
            elif data > self.data:
                if self.right:
                    self.right.insert(data)
                else:
                    self.right = Node(data)
        else:
            self.data = data

    # Root -> Left -> Right
    def preorderTraversal(self, root):
        res = []
        if root:
            res.append(root.data)
            res += self.preorderTraversal(root.left)
            res += self.preorderTraversal(root.right)
        return res

    # Left -> Root -> Right
    def inorderTraversal(self, root):
        res = []
        if root:
            res = self.inorderTraversal(root.left)
            res.append(root.data)
            res += self.inorderTraversal(root.right)
        return res

    # Left -> Right -> Root
    def postorderTraversal(self, root):
        res = []
        if root:
            res = self.postorderTraversal(root.left)
            res += self.postorderTraversal(root.right)
            res.append(root.data)
        return res

    # [BFS] from root L0 -> L1 -> L2
    def levelorderTraversal(self, root):
        res = []
        if root:
            queue = collections.deque()
            queue.append(root)

            # iterate over the queue until its empty
            while queue:
                currSize = len(queue)
                currList = []

                while currSize > 0:
                    currNode = queue.popleft()   # dequeue element
                    currList.append(currNode.data)
                    currSize -= 1

                    if currNode.left:
                        queue.append(currNode.left)

                    if currNode.right:
                        queue.append(currNode.right)

                res += currList
        return res


n = int(input())
root = None
for i in input().split():
    vi = int(i)
    if root:
        root.insert(vi)
    else:
        root = Node(vi)

dict = {}
if root:
    dict[0] = root.preorderTraversal(root)
    dict[1] = root.inorderTraversal(root)
    dict[2] = root.postorderTraversal(root)
    dict[3] = root.levelorderTraversal(root)

for i in range(4):
    line = ' '.join(str(j) for j in dict[i])

    # To debug: print("Debug messages...", file=sys.stderr, flush=True)
    print(line)
