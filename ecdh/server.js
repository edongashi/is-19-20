const crypto = require('crypto')
const { listen } = require('simple-socket-session')
listen(session)

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
  const name = await receive('name')
  console.log(`U pranua mesazh nga ${name}.`)
  const alice = crypto.createECDH('secp521r1')
  const A = alice.generateKeys()
  await send('A', A.toString('hex'))
  const B = await receive('B')
  const secret = alice.computeSecret(B, 'hex')
  console.log({ secret: secret.toString('hex') })

  const key = crypto.pbkdf2Sync(secret, 'salt', 1000, 16, 'sha1')
  const cipherText = encrypt('pershendetje', key)
  await send('cipherText', cipherText)

  const clientCipher = await receive('clientCipher')
  console.log(decrypt(clientCipher, key))
}