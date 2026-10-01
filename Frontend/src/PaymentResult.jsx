import { useEffect, useState } from "react";

function PaymentResult() {
  const [transaction, setTransaction] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const params = new URLSearchParams(
      window.location.search
    );

    const reference = params.get("reference");

    if (!reference) {
      setError("Transaction reference not found.");
      setLoading(false);
      return;
    }

    const verifyPayment = async () => {
      try {
        const response = await fetch(
          `http://localhost:5108/api/checkout/${reference}/verify`
        );

        const data = await response.json();

        if (!response.ok) {
          setError(
            data.message || "Payment verification failed."
          );
          return;
        }

        setTransaction(data);
      } catch (error) {
        console.error(error);

        setError(
          "Unable to verify your payment. Please try again."
        );
      } finally {
        setLoading(false);
      }
    };

    verifyPayment();
  }, []);

  const goToCheckout = () => {
    window.location.href = "/";
  };

  if (loading) {
    return (
      <div className="result-page">
        <div className="result-card loading-card">
          <div className="result-brand">
            <div className="result-brand-symbol">F</div>
            <span>FintechCheckout</span>
          </div>

          <div className="loading-spinner"></div>

          <h1>Verifying your payment</h1>

          <p>
            Please wait while we confirm your transaction.
          </p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="result-page">
        <div className="result-card">
          <div className="result-brand">
            <div className="result-brand-symbol">F</div>
            <span>FintechCheckout</span>
          </div>

          <div className="result-status failed">
            <div className="status-icon">!</div>
            <span>PAYMENT STATUS</span>
          </div>

          <h1>Payment could not be verified</h1>

          <p className="result-description">
            {error}
          </p>

          <button
            className="result-button"
            onClick={goToCheckout}
          >
            Return to checkout
            <span>→</span>
          </button>

          <div className="result-security">
            <strong>Need help?</strong>

            <p>
              If money has been deducted from your account,
              please keep your transaction reference and
              contact support.
            </p>
          </div>
        </div>
      </div>
    );
  }

  const isSuccessful =
    transaction.status === "Success" ||
    transaction.status === 1 ||
    transaction.paystackStatus === "success";

  return (
    <div className="result-page">
      <div className="result-card">

        <div className="result-brand">
          <div className="result-brand-symbol">F</div>
          <span>FintechCheckout</span>
        </div>

        <div
          className={`result-status ${
            isSuccessful ? "success" : "failed"
          }`}
        >
          <div className="status-icon">
            {isSuccessful ? "✓" : "!"}
          </div>

          <span>
            {isSuccessful
              ? "PAYMENT SUCCESSFUL"
              : "PAYMENT FAILED"}
          </span>
        </div>

        <h1>
          {isSuccessful
            ? "Payment completed"
            : "Payment unsuccessful"}
        </h1>

        <p className="result-description">
          {isSuccessful
            ? "Your payment has been successfully processed and verified."
            : "We could not complete your payment. Please check the details below."}
        </p>

        <div className="amount-display">
          <span>Amount paid</span>

          <strong>
            GHS {Number(transaction.amount).toFixed(2)}
          </strong>
        </div>

        <div className="transaction-details">

          <div className="detail-row">
            <span>Transaction reference</span>

            <strong>
              {transaction.reference}
            </strong>
          </div>

          <div className="detail-row">
            <span>Status</span>

            <strong
              className={
                isSuccessful
                  ? "success-text"
                  : "failed-text"
              }
            >
              {isSuccessful ? "Success" : "Failed"}
            </strong>
          </div>

          <div className="detail-row">
            <span>Gateway response</span>

            <strong>
              {transaction.gatewayResponse || "N/A"}
            </strong>
          </div>

          {transaction.paidAt && (
            <div className="detail-row">
              <span>Paid at</span>

              <strong>
                {new Date(
                  transaction.paidAt
                ).toLocaleString()}
              </strong>
            </div>
          )}

        </div>

        <button
          className="result-button"
          onClick={goToCheckout}
        >
          Make another payment
          <span>→</span>
        </button>

        <div className="result-security">
          <div className="security-check">✓</div>

          <div>
            <strong>Secure payment</strong>

            <p>
              This transaction was verified through our
              secure payment gateway.
            </p>
          </div>
        </div>

        <div className="result-footer">
          <span>FintechCheckout</span>
          <span>Powered by Paystack</span>
        </div>

      </div>
    </div>
  );
}

export default PaymentResult;