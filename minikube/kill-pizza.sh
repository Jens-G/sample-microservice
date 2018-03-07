#!/bin/sh

sudo kubectl delete services -l app=pizza-sample
sudo kubectl delete deploy cassandra pizzeria pizzabaker pizza

echo --- PODS WATCH --------------------
sudo kubectl get pods --watch -o wide


