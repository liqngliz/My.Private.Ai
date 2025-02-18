import React, { useState, useEffect, useRef, KeyboardEvent } from 'react';
import './ChatApp.css';

type Message = {
  author_role: "System" | "User" | "Assistant";
  content: string;
};

const ChatApp: React.FC = () => {
  // Holds the initial context (which is not displayed)
  const [initialContext, setInitialContext] = useState<Message[]>([]);
  // Holds the conversation to display (excludes the initial dialogue)
  const [displayedMessages, setDisplayedMessages] = useState<Message[]>([]);
  const [input, setInput] = useState<string>('');
  const [isSending, setIsSending] = useState<boolean>(false);

  // Ref for the chat history container (to scroll to bottom)
  const chatHistoryRef = useRef<HTMLDivElement>(null);

  // Fetch the initial chat conversation from the /New endpoint
  const loadInitialMessages = async () => {
    try {
      const response = await fetch('/New');
      if (!response.ok) {
        throw new Error('Error fetching initial chat');
      }
      const data = await response.json();
      // Save the entire conversation context,
      // but clear any displayed messages so the initial dialogue is hidden.
      setInitialContext(data.messages);
      setDisplayedMessages([]);
    } catch (error) {
      console.error('Error loading initial chat: ', error);
    }
  };

  // Load initial messages when component mounts
  useEffect(() => {
    loadInitialMessages();
  }, []);

  // Scroll to the bottom of the chat history whenever displayedMessages updates
  useEffect(() => {
    if (chatHistoryRef.current) {
      chatHistoryRef.current.scrollTop = chatHistoryRef.current.scrollHeight;
    }
  }, [displayedMessages]);

  // Send a message to /Chat and update the conversation with the response
  const sendMessage = async () => {
    if (!input.trim()) return;
    const messageContent = input;
    // Create the user's message and a placeholder for the assistant's reply
    const userMessage: Message = { author_role: "User", content: messageContent };
    const placeholderMessage: Message = { author_role: "Assistant", content: "...." };

    // Optimistically update the displayed messages with the user's message and placeholder.
    // Note: We do not include the placeholder in the conversation sent to the API.
    setDisplayedMessages(prev => [...prev, userMessage, placeholderMessage]);

    // Build the conversation for the API call using the current context, displayedMessages (without the new placeholder),
    // and the new user message.
    const conversationForAPI: Message[] = [
      ...initialContext,
      ...displayedMessages, // this is the state before our optimistic update
      userMessage,
    ];

    // Clear the input and disable it immediately.
    setInput('');
    setIsSending(true);

    try {
      const response = await fetch('/Chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ messages: conversationForAPI })
      });

      if (!response.ok) {
        throw new Error('Error sending message');
      }

      // Expect the full conversation (including initial context) in the response.
      const data = await response.json();
      const fullConversation: Message[] = data.messages;
      // Remove the initial context so that only the subsequent conversation is displayed.
      const newDisplayedMessages = fullConversation.slice(initialContext.length);
      setDisplayedMessages(newDisplayedMessages);
    } catch (error) {
      console.error('Error sending message: ', error);
    } finally {
      setIsSending(false);
    }
  };

  // Allow sending the message on pressing Enter (without Shift)
  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  return (
    <div className="chat-app">
      <div className="chat-header">
        <button onClick={loadInitialMessages} className="reset-button">
          Reset Chat
        </button>
      </div>
      <div className="chat-history" ref={chatHistoryRef}>
        {displayedMessages.map((msg, index) => (
          <div
            key={index}
            className={`message ${
              msg.author_role === "Assistant" && msg.content === "...." ? "pending" : ""
            }`}
          >
            <strong>{msg.author_role}: </strong>
            <span>{msg.content}</span>
          </div>
        ))}
      </div>
      <div className="chat-input">
        <textarea
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Type your message here..."
          className="text-area"
          disabled={isSending}
        />
        <button onClick={sendMessage} className="send-button" disabled={isSending}>
          Send
        </button>
      </div>
    </div>
  );
};

export default ChatApp;
