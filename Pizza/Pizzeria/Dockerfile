FROM mono:latest
EXPOSE 9090
WORKDIR /work
COPY bin/Debug  .
RUN ls
# RUN mono nuget.exe restore
# RUN xbuild /p:Configuration=Release
# CMD ["mono", "/work/bin/Release/Pizzeria.exe"]
CMD ["mono", "/work/Pizzeria.exe"]
