import '../CSS/Chat.css'
import {useEffect, useRef, useState} from "react";

type ChatProps = {
    room:string;
};


type ChatMessage ={
    user: string;
    message: string;
};


export function Chat({room}: ChatProps) {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [input, setInput] = useState("");
    const [isSomeoneTyping, setIsSomeoneTyping] = useState(false);
    const typingTimeoutRef = useRef<number | null>(null);


    const chatEndRef = useRef(null);


    const username = localStorage.getItem("username");


    // Connect to SSE
    useEffect(() => {
        if (!room) return;
        const eventSource = new EventSource("http://localhost:5050/connect");


        eventSource.addEventListener("connected", (event) => {
            const data = JSON.parse(event.data);


            const token = localStorage.getItem("token");


            // Join room
            fetch("http://localhost:5050/join", {
                method: "POST",
                headers: { "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    connectionId: data.connectionId,
                    room
                })
            });
        });


        eventSource.addEventListener(room, (event) => {
            const data = JSON.parse(event.data);

            if(typeof data.isTyping === "boolean") {
                setIsSomeoneTyping(data.isTyping);
                return;
            }

            if (data.message && data.user) {
                setMessages(prev => [...prev, data]);
            }
            //setMessages(prev => [...prev, data.message ?? data.Message]);
        });


        return () => eventSource.close();
    }, [room]);


    const sendMessage  = async () => {
        if(!input.trim()) return;


        const token = localStorage.getItem("token");


        try {
            await fetch("http://localhost:5050/send", {
                method: "POST",
                headers: {"Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`},
                body: JSON.stringify({
                    room,
                    message: input
                })
            });
            sendTyping(false);
            setInput(""); // clear input after successful send
        } catch (err) {
            console.error("Failed to send message:", err);
        }
    }


    const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") sendMessage();
    }


    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setInput(e.target.value);


        sendTyping(true);


        if (typingTimeoutRef.current) {
            clearTimeout(typingTimeoutRef.current);
        }


        typingTimeoutRef.current = window.setTimeout(() => {
            sendTyping(false);
        }, 1000); // stops after 1s of inactivity
    };


    const sendTyping = async (isTyping: boolean) => {
        const token = localStorage.getItem("token");


        try {
            await fetch("http://localhost:5050/typing", {
                method: "POST",
                headers: { "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`},
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
                    <div key={idx} className={`chat-message ${msg.user === username ? "own" : ""}`}>
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
                {/*<button onClick={() => {*/}


                {/*}}*/}
                {/*>Poke</button>*/}
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
    )
}
