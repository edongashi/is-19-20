const crypto = require('crypto')
const { connect } = require('simple-socket-session')
connect('http://192.168.0.117', session)

function encrypt(text, key) {
  const iv = crypto.randomBytes(16)
  const cipher = crypto.createCipheriv('aes-128-cbc', key, iv);
  let encrypted = cipher.update(text);
  encrypted = Buffer.concat([encrypted, cipher.final()]);
  return { iv: iv.toString('hex'), encryptedData: encrypted.toString('hex') };
}

function decrypt(message, key) {
  let iv = Buffer.from(message.iv, 'hex');
  let encryptedText = Buffer.from(message.encryptedData, 'hex');
  let decipher = crypto.createDecipheriv('aes-128-cbc', Buffer.from(key), iv);
  let decrypted = decipher.update(encryptedText);
  decrypted = Buffer.concat([decrypted, decipher.final()]);
  return decrypted.toString();
}

async function session({ send, receive }) {
  await send('name', 'edon')
  const bob = crypto.createECDH('secp521r1')
  const B = bob.generateKeys()
  const A = await receive('A')
  await send('B', B.toString('hex'))
  const secret = bob.computeSecret(A, 'hex')
  console.log({ secret: secret.toString('hex') })
  
  const key = crypto.pbkdf2Sync(secret, 'salt', 1000, 16, 'sha1')
  const cipherText = await receive('cipherText')
  const plainText = decrypt(cipherText, key)
  console.log({ plainText })

  await send('clientCipher', encrypt('klienti', key))
}