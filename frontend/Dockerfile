# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build argument for API URL (defaults to localhost for local Docker, override for production)
ARG API_URL=http://taskmanager-api:8080/api
ENV API_URL=$API_URL

# Update environment.ts with API URL at build time
RUN sed -i "s|apiBaseUrl:.*|apiBaseUrl: '${API_URL}',|g" src/environments/environment.ts && \
    sed -i "s|production:.*|production: true,|g" src/environments/environment.ts

# Build the application
RUN npm run build

# Production stage
FROM nginx:1.25-alpine

# Copy built app from build stage (Angular 18 outputs to browser subdirectory)
COPY --from=build /app/dist/taskmanager-frontend/browser /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
