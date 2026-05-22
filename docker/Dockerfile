FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/GroupDocs.Redaction.Mcp/GroupDocs.Redaction.Mcp.csproj \
    -c Release -o /app --no-self-contained

FROM base AS final

# Native graphics deps GroupDocs.Redaction's image-area redaction path needs on
# Linux. libgdiplus is the GDI+ implementation System.Drawing.Common P/Invokes
# for drawing solid-color rectangular boxes over page regions; libfontconfig1 is
# its hidden font dependency. Pair this with <RuntimeHostConfigurationOption
# Include="System.Drawing.EnableUnixSupport" Value="true" /> in the csproj —
# without both, image-area redaction throws PlatformNotSupportedException on Linux.
# ttf-mscorefonts is NOT needed: Redaction does structural redaction and pixel-box
# overlays, not text-glyph rendering.
USER root
RUN apt-get update \
 && apt-get install -y --no-install-recommends libgdiplus libfontconfig1 \
 && rm -rf /var/lib/apt/lists/*

# OCI image labels. VERSION is supplied by the publish workflow via
# --build-arg VERSION=YY.MM.N. Default 'dev' makes local `docker build` runs
# self-identify instead of inheriting the base image's misleading version.
ARG VERSION=dev
LABEL org.opencontainers.image.title="GroupDocs.Redaction.Mcp" \
      org.opencontainers.image.description="MCP server for GroupDocs.Redaction — redact text, metadata, annotations, and image areas in documents via AI agents." \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.source="https://github.com/groupdocs-redaction/GroupDocs.Redaction.Mcp" \
      org.opencontainers.image.url="https://www.nuget.org/packages/GroupDocs.Redaction.Mcp" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.vendor="Aspose Pty Ltd"

COPY --from=build /app .

ENV GROUPDOCS_MCP_STORAGE_PATH=/data
VOLUME /data

USER $APP_UID

ENTRYPOINT ["dotnet", "GroupDocs.Redaction.Mcp.dll"]
