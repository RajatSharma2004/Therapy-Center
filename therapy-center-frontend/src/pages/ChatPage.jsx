import ChatBox from '../components/ChatBox'

export default function ChatPage() {
  return (
    <div>
      <div className="page-header">
        <h1>Staff Chat</h1>
        <p>Real-time messaging between Admin and Receptionist</p>
      </div>
      <ChatBox />
    </div>
  )
}