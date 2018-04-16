#!/bin/sh
#sudo kubeadm reset
sudo systemctl stop localkube
sudo kubeadm init --pod-network-cidr=10.244.0.0/16
#eof
