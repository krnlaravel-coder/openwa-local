import fs from 'fs';
import path from 'path';
import readline from 'readline';

// Auto-read the API key from the data directory
const apiKeyPath = path.join(process.cwd(), 'data', '.api-key');
let apiKey = '';

try {
  apiKey = fs.readFileSync(apiKeyPath, 'utf8').trim();
  console.log(`🔑 Automatically loaded API key from: ${apiKeyPath}`);
} catch (err) {
  console.error(`❌ Error reading API key from ${apiKeyPath}:`, err.message);
  process.exit(1);
}

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

const askQuestion = (query) => new Promise((resolve) => rl.question(query, resolve));

async function main() {
  console.log('\n--- OpenWA Message Sender Test Tool ---');
  
  // 1. Get session name
  const sessionName = (await askQuestion('Enter session name (default: my-session): ')).trim() || 'my-session';
  
  // 2. Get receiver phone number
  const rawPhone = await askQuestion('Enter recipient phone number (with country code, e.g. 15550123456): ');
  const phone = rawPhone.replace(/\D/g, '').trim();
  if (!phone) {
    console.error('❌ Recipient phone number is required.');
    rl.close();
    return;
  }
  
  // Format as WhatsApp chat ID: phone@c.us
  const chatId = `${phone}@c.us`;
  
  // 3. Get message content
  const text = (await askQuestion('Enter message text (default: Hello from OpenWA!): ')).trim() || 'Hello from OpenWA!';
  
  rl.close();
  
  const url = `http://localhost:2785/api/sessions/${sessionName}/messages/send-text`;
  const body = JSON.stringify({ chatId, text });
  
  console.log(`\n📤 Sending message to ${chatId}...`);
  console.log(`   URL: ${url}`);
  console.log(`   Payload: ${body}\n`);
  
  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-api-key': apiKey
      },
      body
    });
    
    const result = await response.json();
    
    if (response.ok) {
      console.log('✅ Message sent successfully!');
      console.log('Response:', JSON.stringify(result, null, 2));
    } else {
      console.error(`❌ Failed to send message (HTTP ${response.status}):`);
      console.error(JSON.stringify(result, null, 2));
      console.log('\n💡 Tip: Make sure your session is active and linked (connected) in the dashboard!');
    }
  } catch (error) {
    console.error('❌ Network error during request:', error.message);
  }
}

main();
