import { useState } from "react";
import PaymentResult from "./PaymentResult";
import "./App.css";

function App() {
  const [email, setEmail] = useState("");
  const [amount, setAmount] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (event) => {
    event.preventDefault();

    setLoading(true);
    setError("");

    try {
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL}/api/checkout`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            email,
            amount: Number(amount),
          }),
        }
      );

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.message || "Unable to start payment."
        );
      }

      window.location.href = data.authorizationUrl;
    } catch (error) {
      console.error("Checkout failed:", error);

      setError(
        error.message || "Unable to start payment."
      );

      setLoading(false);
    }
  };

  if (window.location.pathname === "/payment-result") {
    return <PaymentResult />;
  }

  return (
    <div className="checkout-page">
      <header className="topbar">
        <div className="brand">
          <div className="brand-symbol">
            <span></span>
            <span></span>
          </div>

          <div>
            <div className="brand-name">FintechCheckout</div>
            <div className="brand-caption">
              Secure payment platform
            </div>
          </div>
        </div>

        <div className="security-label">
          <span className="security-dot"></span>
          Secure checkout
        </div>
      </header>

      <main className="main-content">
        <section className="brand-panel">
          <div className="orange-shape orange-shape-one"></div>
          <div className="orange-shape orange-shape-two"></div>

          <div className="brand-content">
            <span className="section-label">
              DIGITAL PAYMENTS
            </span>

            <h1>
              Simple payments.
              <br />
              <span>Built for trust.</span>
            </h1>

            <p>
              Make secure payments quickly and confidently.
              Your transaction is protected from checkout
              through confirmation.
            </p>

            <div className="feature-list">
              <div className="feature">
                <div className="feature-icon">✓</div>

                <div>
                  <strong>Secure</strong>
                  <span>Protected payment processing</span>
                </div>
              </div>

              <div className="feature">
                <div className="feature-icon">✓</div>

                <div>
                  <strong>Fast</strong>
                  <span>Complete your payment in seconds</span>
                </div>
              </div>

              <div className="feature">
                <div className="feature-icon">✓</div>

                <div>
                  <strong>Reliable</strong>
                  <span>Real-time payment verification</span>
                </div>
              </div>
            </div>
          </div>

          <div className="panel-footer">
            <span>GHS</span>
            <span>Powered by Paystack</span>
          </div>
        </section>

        <section className="form-panel">
          <div className="form-container">
            <div className="form-heading">
              <span className="form-label">CHECKOUT</span>

              <h2>Complete your payment</h2>

              <p>
                Enter your details to continue securely.
              </p>
            </div>

            <form
              className="payment-form"
              onSubmit={handleSubmit}
            >
              <div className="input-group">
                <label htmlFor="email">
                  Email address
                </label>

                <input
                  id="email"
                  type="email"
                  placeholder="customer@example.com"
                  value={email}
                  onChange={(event) =>
                    setEmail(event.target.value)
                  }
                  required
                />
              </div>

              <div className="input-group">
                <label htmlFor="amount">
                  Payment amount
                </label>

                <div className="currency-input">
                  <span>GHS</span>

                  <input
                    id="amount"
                    type="number"
                    placeholder="150.00"
                    value={amount}
                    onChange={(event) =>
                      setAmount(event.target.value)
                    }
                    min="0.01"
                    step="0.01"
                    required
                  />
                </div>
              </div>

              {error && (
                <div className="error-message">
                  {error}
                </div>
              )}

              <div className="payment-summary">
                <div>
                  <span>Total payment</span>
                  <small>Ghana Cedis</small>
                </div>

                <strong>
                  GHS{" "}
                  {amount
                    ? Number(amount).toFixed(2)
                    : "0.00"}
                </strong>
              </div>

              <button
                type="submit"
                className="pay-button"
                disabled={loading}
              >
                <span>
                  {loading
                    ? "Processing..."
                    : "Continue to payment"}
                </span>

                {!loading && (
                  <span className="arrow">→</span>
                )}
              </button>
            </form>

            <div className="form-security">
              <div className="lock-icon">✓</div>

              <div>
                <strong>Secure transaction</strong>

                <p>
                  Your payment is securely processed by
                  Paystack. We never store your card details.
                </p>
              </div>
            </div>

            <div className="form-footer">
              <span>FintechCheckout</span>
              <span>Paystack</span>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}

export default App;