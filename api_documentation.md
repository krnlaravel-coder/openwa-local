# WhtsApi Local Server API Documentation

The `WhtsApi` application not only provides a UI for logging into WhatsApp, but also hosts a local HTTP server (by default on `http://localhost:5000`) that allows you to send messages to WhatsApp from other applications or scripts.

## Base URL
`http://localhost:5000`

## Authentication
Currently, the local server does not require authentication since it runs strictly on `localhost`. Make sure to secure your network if exposing this port to other machines.

---

## 1. Send Text Message
Sends a standard text message to a specified number.

**Endpoint:** `POST /api/send/text`

**Headers:**
`Content-Type: application/json`

**Payload Example:**
```json
{
  "number": "1234567890",
  "message": "Hello from WhtsApi!"
}
```
*Note: The `number` should include the country code without the `+` or spaces.*

---

## 2. Send Document (File)
Sends a document (PDF, DOCX, etc.) to a specified number using a base64 encoded string or a direct URL.

**Endpoint:** `POST /api/send/document`

**Headers:**
`Content-Type: application/json`

**Payload Example:**
```json
{
  "number": "1234567890",
  "filename": "invoice.pdf",
  "documentUrl": "https://example.com/invoice.pdf",
  "caption": "Please find the invoice attached."
}
```

---

## 3. Send Link
Sends a URL link to a specified number. (Note: In WhatsApp, links are just text messages, but we provide this endpoint for semantic clarity if your OpenWA server supports rich link previews).

**Endpoint:** `POST /api/send/link`

**Headers:**
`Content-Type: application/json`

**Payload Example:**
```json
{
  "number": "1234567890",
  "url": "https://google.com",
  "message": "Check out this link!"
}
```

---

## Error Responses
If the WhtsApi application is not connected to WhatsApp (e.g., QR not scanned), it will return a `503 Service Unavailable` error:

```json
{
  "error": "WhatsApp session is not active. Please scan the QR code in the WhtsApi application."
}
```
