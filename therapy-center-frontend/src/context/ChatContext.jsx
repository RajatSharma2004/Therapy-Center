import { createContext, useContext, useState, useEffect, useCallback } from 'react'
import * as signalR from '@microsoft/signalr'
import { useAuth } from './AuthContext'

const ChatContext = createContext(null)

export function ChatProvider({ children }) {
  const { user } = useAuth()
  const [connection, setConnection] = useState(null)
  const [messages, setMessages] = useState([])
  const [connectedUsers, setConnectedUsers] = useState([])
  const [isConnected, setIsConnected] = useState(false)

  useEffect(() => {
    if (!user || (user.role !== 'Admin' && user.role !== 'Receptionist')) return

    const conn = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/chat', {
     accessTokenFactory: () => localStorage.getItem('token')
    })
      .withAutomaticReconnect()
      .build()

    conn.on('ReceiveMessage', (msg) => {
      setMessages((prev) => [...prev, msg])
    })

    conn.on('MessageHistory', (history) => {
      setMessages(history)
    })

    conn.on('UserJoined', (msg) => {
      setConnectedUsers((prev) => [...prev, msg])
    })

    conn.on('UserLeft', (msg) => {
      setConnectedUsers((prev) => prev.filter((u) => u !== msg))
    })

    conn.onreconnecting(() => setIsConnected(false))
    conn.onreconnected(() => setIsConnected(true))
    conn.onclose(() => setIsConnected(false))

    conn.start()
      .then(() => setIsConnected(true))
      .catch((err) => console.error('SignalR connection error:', err))

    setConnection(conn)

    return () => {
      conn.stop()
    }
  }, [user])

  const sendMessage = useCallback(
    async (text) => {
      if (connection && text.trim()) {
        try {
          await connection.invoke('SendMessage', text)
        } catch (err) {
          console.error('SendMessage error:', err)
        }
      }
    },
    [connection]
  )

  return (
    <ChatContext.Provider value={{ messages, connectedUsers, isConnected, sendMessage }}>
      {children}
    </ChatContext.Provider>
  )
}

export function useChat() {
  const ctx = useContext(ChatContext)
  if (!ctx) {
    return { messages: [], connectedUsers: [], isConnected: false, sendMessage: () => {} }
  }
  return ctx
}