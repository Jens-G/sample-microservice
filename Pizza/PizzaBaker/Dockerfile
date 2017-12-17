FROM mono:latest
EXPOSE 9091
WORKDIR /work
COPY bin/Debug  .
RUN ls
CMD ["mono", "/work/PizzaBaker.exe"]

# RUN mono nuget.exe restore
# RUN xbuild /p:Configuration=Release
# CMD ["mono", "/work/bin/Release/PizzaBaker.exe"]

