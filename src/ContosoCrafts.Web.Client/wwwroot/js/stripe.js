export function checkout(pubKey, sessionId) {
    let stripe = Stripe(pubKey);        
    stripe.redirectToCheckout({
        sessionId
    });
}