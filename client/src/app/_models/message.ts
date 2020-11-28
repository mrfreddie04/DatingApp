export interface Message {
    id: number;
    senderUsername: string;
    recipientUsername: string;
    senderId: number;
    senderPhotoUrl: string;
    recipientId: number;
    recipientPhotoUrl: string;
    content: string;
    dateRead?: Date;
    dateSent: Date;
}