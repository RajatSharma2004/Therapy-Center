import { useState } from 'react'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import { auth } from '../api/api'
import { useAuth } from '../context/AuthContext'

const ROLE_HOME = {
  Admin: '/admin',
  Receptionist: '/staff',
  Doctor: '/doctor',
  Patient: '/patient',
  Guardian: '/patient',
}

export default function VerifyOtpPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { login } = useAuth()

  const [email, setEmail] = useState(location.state?.email ?? '')
  const [purpose, setPurpose] = useState(location.state?.purpose ?? 'Register')
  const [otp, setOtp] = useState('')
  const [error, setError] = useState('')
  const [message, setMessage] = useState(location.state?.message ?? '')
  const [loading, setLoading] = useState(false)
  const [resending, setResending] = useState(false)

  async function handleVerify(e) {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const data = await auth.verifyOtp({ email, purpose, otp })
      login(data)
      const role = data.role ?? data.Role
      navigate(ROLE_HOME[role] || '/')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  async function handleResend() {
    setError('')
    setMessage('')
    setResending(true)

    try {
      const data = await auth.resendOtp({ email, purpose })
      setMessage(data.message || 'OTP resent successfully.')
    } catch (err) {
      setError(err.message)
    } finally {
      setResending(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">🔐</div>
        <h1>Verify OTP</h1>
        <p>Enter the OTP sent to your email.</p>

        {message && <div className="alert alert-success">{message}</div>}
        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleVerify}>
          <div className="form-group">
            <label>Email</label>
            <input value={email} onChange={e => setEmail(e.target.value)} required />
          </div>

          <input type="hidden" value={purpose} />

          <div className="form-group">
            <label>OTP</label>
            <input
              value={otp}
              onChange={e => setOtp(e.target.value)}
              placeholder="6-digit code"
              maxLength={6}
              required
            />
          </div>

          <button className="btn btn-primary" style={{ width: '100%' }} disabled={loading}>
            {loading ? 'Verifying…' : 'Verify OTP'}
          </button>
        </form>

        <button
          className="btn btn-secondary"
          style={{ width: '100%', marginTop: 12 }}
          onClick={handleResend}
          disabled={resending}
        >
          {resending ? 'Resending…' : 'Resend OTP'}
        </button>

        <div className="auth-footer">
          <Link to="/login">Back to login</Link>
        </div>
      </div>
    </div>
  )
}