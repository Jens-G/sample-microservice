#!/bin/sh
clear

sudo kubectl create -f service-$1

echo --- SERVICES ------------------------
sudo kubectl get services
echo --- DEPLOY --------------------------
sudo kubectl get deploy
echo --- PODS ----------------------------
sudo kubectl get pods -o wide

#eof
