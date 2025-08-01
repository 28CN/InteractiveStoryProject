# =========== 构建阶段 ===========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制解决方案和项目文件
COPY InteractiveStoryProject.sln .
COPY InteractiveStoryApi/*.csproj ./InteractiveStoryApi/
COPY LinkListStory/*.csproj ./LinkListStory/

# 还原 NuGet 依赖
RUN dotnet restore InteractiveStoryProject.sln

# 复制所有源代码（包括 story.json）
COPY . .

# 发布 API 项目
RUN dotnet publish InteractiveStoryApi/InteractiveStoryApi.csproj -c Release -o /app/publish

# =========== 运行阶段 ===========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 复制发布结果
COPY --from=build /app/publish .

# ✅ 复制 story.json 到容器指定路径
COPY LinkListStory/story.json /app/data/story.json

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "InteractiveStoryApi.dll"]
