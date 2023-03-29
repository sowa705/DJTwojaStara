#!/bin/bash

### This script will download the model and compile the code for the AI chatbot feature

# Crash on error
set -e

# Clone the tcp_server branch of the llama.cpp repository into /tmp/djtwojastara-temp
git clone -b tcp_server https://github.com/tarruda/llama.cpp/ /tmp/djtwojastara-temp/llama.cpp

# Navigate into the cloned repository
cd /tmp/djtwojastara-temp/llama.cpp

# Run the make command to compile the code
make

cd models

# Alpaca 13B - recommended model
model_url="https://drive.google.com/uc?id=1E42QuujXnYVcsdyrEC1g0FMv0eGbE_u1"

# Download the model file using gdown
gdown "$model_url"

# copy the unit file to the systemd directory
cp inference.service /usr/lib/systemd/system/inference.service

# Reload the systemd daemon and enable the service
systemctl daemon-reload
systemctl enable inference.service
systemctl start inference.service