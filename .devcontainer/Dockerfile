FROM mcr.microsoft.com/dotnet/sdk:9.0

ENV DEBIAN_FRONTEND=noninteractive

RUN curl -sSL https://github.com/bufbuild/buf/releases/latest/download/buf-Linux-x86_64 -o /usr/local/bin/buf \
    && chmod +x /usr/local/bin/buf

# Create a non-root user
RUN groupadd -g 1000 vscode \
    && useradd -u 1000 -g 1000 -m -s /bin/bash vscode \
    && usermod -aG sudo vscode \
    && echo "vscode ALL=(ALL) NOPASSWD: ALL" >> /etc/sudoers

ENV CGO_ENABLED=1

WORKDIR /workspace
USER vscode
CMD ["/bin/bash"]
