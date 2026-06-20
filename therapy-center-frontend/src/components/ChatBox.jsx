import { useState, useRef, useEffect } from 'react'
import { useChat } from '../context/ChatContext'
import { useAuth } from '../context/AuthContext'

export default function ChatBox() {
  const { messages, connectedUsers, isConnected, sendMessage } = useChat()
  const { user } = useAuth()
  const [input, setInput] = useState('')
  const messagesEndRef = useRef(null)

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  async function handleSubmit(e) {
    e.preventDefault()
    if (!input.trim()) return
    await sendMessage(input)
    setInput('')
  }

  return (
    <div className="chat-container">
      {/* Header */}
      <div className="chat-header">
        <span>💬 Staff Chat</span>
        <span className={`chat-status ${isConnected ? 'connected' : 'disconnected'}`}>
          {isConnected ? '🟢 Connected' : '🔴 Disconnected'}
        </span>
      </div>

      {/* Connected users */}
      {connectedUsers.length > 0 && (
        <div className="chat-users-bar">
          {connectedUsers.join(' · ')}
        </div>
      )}

      {/* Messages */}
      <div className="chat-messages">
        {messages.length === 0 && (
          <div className="chat-empty">No messages yet. Start the conversation!</div>
        )}
        {messages.map((msg, i) => {
          const isOwn = msg.senderId === user?.userId
          return (
            <div key={msg.id ?? i} className={`chat-bubble ${isOwn ? 'own' : 'other'}`}>
              {!isOwn && (
                <div className="chat-bubble-sender">
                 {msg.senderName ??
                  `${msg.sender?.firstName ?? ''} ${msg.sender?.lastName ?? ''}`.trim()}
                </div>
                )}
              <div>{msg.message}</div>
              <div className="chat-bubble-time">
                {msg.sentAt ? new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : ''}
              </div>
            </div>
          )
        })}
        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <form className="chat-input-bar" onSubmit={handleSubmit}>
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Type a message..."
          className="chat-input"
        />
        <button
          type="submit"
          disabled={!input.trim() || !isConnected}
          className="chat-send-btn"
        >
          Send
        </button>
      </form>
    </div>
  )
}