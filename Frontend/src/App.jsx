import { useState } from "react";
import PaymentResult from "./PaymentResult";

function App() {
  const [email, setEmail] = useState("");
  const [amount, setAmount] = useState("");

  const handleSubmit = async (event) => {
    event.preventDefault();

    try {
      const response = await fetch("http://localhost:5108/api/checkout", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email,
          amount: Number(amount),
        }),
      });

      const data = await response.json();

      window.location.href = data.authorizationUrl;
    } catch (error) {
      console.error("Checkout failed:", error);
    }
  };

  if (window.location.pathname === "/payment-result") {
    return <PaymentResult />;
  }

  return (
    <div>
      <h1>FintechCheckout</h1>
      <p>Secure payment checkout</p>

      <form onSubmit={handleSubmit}>
        <div>
          <label>Email</label>
          <input
            type="email"
            placeholder="customer@example.com"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
          />
        </div>

        <div>
          <label>Amount (GHS)</label>
          <input
            type="number"
            placeholder="150"
            value={amount}
            onChange={(event) => setAmount(event.target.value)}
            min="0.01"
            step="0.01"
            required
          />
        </div>

        <button type="submit">Pay Now</button>
      </form>
    </div>
  );
}

export default App;
