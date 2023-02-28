#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import socket
import sys
import base64
from urllib.parse import urlparse

HOST = '0.0.0.0'  # Standard loopback interface address (localhost)
PORT = 53534        # Port to listen on (non-privileged ports are > 1023)

f = open('schema.json')
text = f.read()
f.close()
print(text)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()
    while True:
        conn, addr = s.accept()
        with conn:
            print('Connected by', addr)

            try:

                data = ''
                while True:
                    buffer = conn.recv(100)
                    data += buffer.decode('ascii')
                    if len(buffer) < 100:
                        break

                print(data.split('\n\n', 1))

                head, body = data.split('\n\n', 1)
                body = base64.b64decode(body.encode('ascii')).decode('utf-8')
                articles = head.split('\n')
                if urlparse(articles[1]).path == '/main.3dml':
                    f = open('schema.json')
                    text = f.read()
                    text = '3D.NET/0.2 200 OK\nContent-Type: application/json\n\n' + base64.b64encode(text.encode('utf-8')).decode('ascii')
                    f.close()
                elif urlparse(articles[1]).path == '/object.obj':
                    f = open('cyberia.obj')
                    text = f.read()
                    text = '3D.NET/0.2 200 OK\nContent-Type: application/obj\n\n' + base64.b64encode(text.encode('utf-8')).decode('ascii')
                    f.close()
                elif urlparse(articles[1]).path == '/object.jpg':
                    f = open('pic.jpg', 'rb')
                    text = f.read()
                    text = '3D.NET/0.2 200 OK\nContent-Type: image/jpeg\n\n' + base64.b64encode(text).decode('ascii')
                    f.close()
                conn.send(text.encode('ascii'))
            except Exception as e:
                conn.close()
                print('On line '+str(sys.exc_info()[-1].tb_lineno)+': '+str(e))

            print()
