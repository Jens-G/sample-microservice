#!/bin/sh
echo Installing flannel ...
sudo kubectl apply -f https://raw.githubusercontent.com/coreos/flannel/master/Documentation/kube-flannel.yml
#eof
