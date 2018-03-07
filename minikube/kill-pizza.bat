@echo off

kubectl delete services -l app=pizza-sample
kubectl delete deploy cassandra pizzeria pizzabaker pizza

echo --- PODS (WATCH) --------------------
kubectl get pods --watch -o wide
