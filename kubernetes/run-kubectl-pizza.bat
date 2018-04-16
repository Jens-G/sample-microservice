@echo off
cls

kubectl create -f service-%1

echo --- SERVICES ------------------------
kubectl get services
echo --- DEPLOY --------------------------
kubectl get deploy
echo --- PODS (WATCH) --------------------
kubectl get pods -o wide

echo.
echo Waiting for service IP ...
kubectl get service --watch

:eof