export function checkout(pubKey, sessionId) {
  console.log(`Checking out with session: ${sessionId}`);
  const stripe = Stripe(pubKey);
  stripe.redirectToCheckout({ sessionId });
}
