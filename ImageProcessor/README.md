# Image Processing API

A .NET 8 Web API for processing and serving images with automatic resizing, format conversion, and EXIF metadata extraction.

## Features

- Upload images (JPG, PNG, WebP)
- Automatic conversion to WebP format
- Automatic resizing for different devices (phone, tablet, desktop)
- EXIF metadata extraction
- Image metadata storage and retrieval
- File-based storage system

## Requirements

- .NET 8 SDK
- Visual Studio 2022 or VS Code

## Setup

1. Clone the repository
2. Navigate to the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## API Endpoints

### Upload Image
```http
POST /api/images
Content-Type: multipart/form-data

file: <image-file>
```

### Get Resized Image
```http
GET /api/images/{id}/{size}
```
Sizes: phone, tablet, desktop

### Get Image Metadata
```http
GET /api/images/{id}/metadata
```

### Delete Image
```http
DELETE /api/images/{id}
```

## File Size Limits
- Individual files: 2MB
- Request size limit: 10MB

## Image Dimensions
- Phone: 640x960
- Tablet: 1024x1536
- Desktop: 1920x2880

## Storage Structure
```
storage/
└── {imageId}/
    ├── original.[jpg|png|webp]
    ├── phone.webp
    ├── tablet.webp
    ├── desktop.webp
    └── metadata.json
```

## Dependencies

- SixLabors.ImageSharp: Image processing
- MetadataExtractor: EXIF data extraction 