import { useEffect, useState } from "react";

function PaymentResult() {
  const [transaction, setTransaction] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const reference = params.get("reference");

    if (!reference) {
      setError("Transaction reference not found.");
      return;
    }

    const verifyPayment = async () => {
      try {
        const response = await fetch(
          `http://localhost:5108/api/checkout/${reference}/verify`,
        );

        const data = await response.json();

        if (!response.ok) {
          setError(data.message || "Payment verification failed.");
          return;
        }

        setTransaction(data);
      } catch (error) {
        console.error(error);
        setError("Unable to verify payment.");
      }
    };

    verifyPayment();
  }, []);

  if (error) {
    return (
      <div>
        <h1>Payment Failed</h1>
        <p>{error}</p>
      </div>
    );
  }

  if (!transaction) {
    return (
      <div>
        <h1>Verifying Payment...</h1>
        <p>Please wait while we confirm your payment.</p>
      </div>
    );
  }

  return (
    <div>
      <h1>
        {transaction.status === 1 ? "Payment Successful" : "Payment Failed"}
      </h1>

      <p>Amount: GHS {transaction.amount}</p>

      <p>Reference: {transaction.reference}</p>

      <p>Status: {transaction.status === 1 ? "Success" : "Failed"}</p>

      <p>Gateway Response: {transaction.gatewayResponse}</p>
    </div>
  );
}

export default PaymentResult;
