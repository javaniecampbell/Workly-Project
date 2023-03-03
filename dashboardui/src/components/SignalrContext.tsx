import { HubConnectionBuilder } from '@microsoft/signalr';
import React, { createContext, PropsWithChildren, useEffect } from 'react';
import dotenv from 'dotenv';

dotenv.config();


interface SignalrCtx {

}

export const SignalrContext = createContext<SignalrCtx>({

});

function SignalrProvider({ children }: PropsWithChildren) {

    useEffect(() => {

        if (process.env.HUB_URL === undefined) {
            console.error("Hub is not set");
            return;
        }
        const connection = new HubConnectionBuilder()
            .withUrl(process.env.HUB_URL)
            .withAutomaticReconnect()
            .build();

        connection.on("OnConnected", (payload) => {

        });


        connection.start().then(result => {

        }).catch(reason => {
            console.error(reason);
        })

        return () => {

        }
    }, [])

    return (
        <SignalrContext.Provider value={{}}>
            {children}
        </SignalrContext.Provider>
    );
}

export default SignalrProvider;