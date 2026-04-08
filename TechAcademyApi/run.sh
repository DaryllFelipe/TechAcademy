#!/bin/bash

# TechAcademy API - Quick Setup and Run Script

echo "Cleaning previous build..."
dotnet clean

echo ""
echo "Restoring NuGet packages..."
dotnet restore

echo ""
echo "Building project..."
dotnet build

echo ""
echo "Running application..."
echo ""
echo "============================================"
echo "TechAcademy API is starting..."
echo "============================================"
echo ""
echo "Swagger UI will be available at:"
echo "https://localhost:5001/swagger"
echo ""
echo "Or HTTP: http://localhost:5000"
echo ""

dotnet run
