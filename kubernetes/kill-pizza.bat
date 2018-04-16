@echo off

kubectl delete services -l app=pizza-sample
kubectl delete deploy cassandra pizzeria pizzabaker pizza sqlserver

echo --- PODS (WATCH) --------------------
kubectl get pods --watch -o wide
