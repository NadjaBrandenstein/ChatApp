import '../CSS/Chat.css'
import { useEffect, useRef, useState } from "react";
import { useStream } from "../Hooks/useStream";

type ChatMessage = {
    user: string;
    message: string;
};

export function Chat({ room }: { room: string }) {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [input, setInput] = useState("");
    const [isSomeoneTyping, setIsSomeoneTyping] = useState(false);

    const typingTimeoutRef = useRef<number | null>(null);
    const chatEndRef = useRef<HTMLDivElement | null>(null);

    const stream = useStream();
    const username = sessionStorage.getItem("username");
    const token = sessionStorage.getItem("token");

    // Join room when connected
    useEffect(() => {
        if (!room || !stream.isConnected) return;
        if (!stream.connectionId) return;

        fetch("http://localhost:5050/join", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                connectionId: stream.connectionId,
                room,
                username
            })
        });
    }, [room, stream.isConnected, stream.connectionId]);

    // Listen for chat messages
    useEffect(() => {
        if (!room) return;

        const cleanupChat = stream.on<ChatMessage>(
            room,
            "ChatResponse",
            (msg) => {
                setMessages(prev => [...prev, msg]);
            }
        );

        const cleanupTyping = stream.on<{ user: string; isTyping: boolean }>(
            room,
            "TypingResponse",
            (data) => {
                if (data.user !== username) {
                    setIsSomeoneTyping(data.isTyping);
                }
            }
        );

        return () => {
            cleanupChat();
            cleanupTyping();
        };
    }, [room, stream]);

    // Send message
    const sendMessage = async () => {
        if (!input.trim() || !username) return;
        console.log("Sending message as user:", username);
        try {
            await fetch("http://localhost:5050/send", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    room,
                    message: input
                })
            });

            sendTyping(false);
            setInput("");
        } catch (err) {
            console.error("Failed to send message:", err);
        }
    };

    //Load previous messages when entering room
    useEffect(() => {
        if(!room) {
            return;
        }

        const loadMessages = async () => {
            try{
                const res = await fetch(`http://localhost:5050/messages/${room}`);

                if(!res.ok){
                    return;
                }

                const data = await res.json();

                setMessages(data.reverse());
            }
            catch(err){
                console.error("Failed to load message:", err);
            }
        };
        loadMessages();
    },[room])

    const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") sendMessage();
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setInput(e.target.value);

        sendTyping(true);

        if (typingTimeoutRef.current) {
            clearTimeout(typingTimeoutRef.current);
        }

        typingTimeoutRef.current = window.setTimeout(() => {
            sendTyping(false);
        }, 1000);
    };

    const sendTyping = async (isTyping: boolean) => {
        try {
            await fetch("http://localhost:5050/typing", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    room,
                    isTyping
                })
            });
        } catch {
            // ignore typing failures
        }
    };

    return (
        <div className="chat-container">
            <div className="chat-box">
                {messages.map((msg, idx) => (
                    <div
                        key={idx}
                        className={`chat-message ${msg.user === username ? "own" : ""}`}
                    >
                        <div className="chat-user">{msg.user}</div>
                        <div className="chat-bubble">{msg.message}</div>
                    </div>
                ))}

                {isSomeoneTyping && (
                    <div className="typing-indicator">
                        Someone is typing...
                    </div>
                )}

                <div ref={chatEndRef} />
            </div>

            <div className="chat-input">
                <input
                    type="text"
                    value={input}
                    onChange={handleChange}
                    onKeyDown={handleKeyPress}
                    placeholder="Write a message..."
                />
                <button onClick={sendMessage}>Send</button>
            </div>
        </div>
    );
}
