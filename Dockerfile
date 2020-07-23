FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 as builder

COPY dotnet/ ./src/
RUN msbuild /p:Configuration=Release src/JobObjectInfo.sln

FROM mcr.microsoft.com/windows:1809

COPY --from=builder ["/src/bin/Release/JobObjectInfo.exe", "."]
CMD ["JobObjectInfo.exe"]