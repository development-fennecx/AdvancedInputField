package com.jeroenvanpienbroek.nativekeyboard;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

import com.google.android.gms.auth.api.phone.SmsRetriever;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.Status;

interface SMSBroadcastReceiverListener
{
    void onSuccess(Intent intent);
    void onFailure();
}

public class SMSBroadcastReceiver extends BroadcastReceiver
{
    public SMSBroadcastReceiverListener smsBroadcastReceiverListener;

    @Override
    public void onReceive(Context context, Intent intent)
    {
        if(intent == null){return;}

        if(intent.getAction() == SmsRetriever.SMS_RETRIEVED_ACTION)
        {
            Bundle extras = intent.getExtras();
            if(extras == null){return;}

            Status smsRetrieverStatus = (Status)extras.get(SmsRetriever.EXTRA_STATUS);
            int statusCode = smsRetrieverStatus.getStatusCode();
            switch(statusCode)
            {
                case CommonStatusCodes.SUCCESS:
                    Intent it = extras.getParcelable(SmsRetriever.EXTRA_CONSENT_INTENT);
                    smsBroadcastReceiverListener.onSuccess(it);
                    break;
                case CommonStatusCodes.TIMEOUT:
                    smsBroadcastReceiverListener.onFailure();
                    break;
            }
        }
    }
}