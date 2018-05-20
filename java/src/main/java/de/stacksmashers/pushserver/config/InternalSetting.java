package de.stacksmashers.pushserver.config;

import de.stacksmashers.pushserver.exceptions.NoInitiationException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

public class InternalSetting {

    private static final Logger logger = LogManager.getLogger(InternalSetting.class);
    private static String uic_id;


    public static String getUic_id() throws NoInitiationException {
        if(uic_id==null){
            throw new NoInitiationException("Not initialized yet!");
        }
        return uic_id;
    }

    public static void setUic_id(String uic_id) {
        InternalSetting.uic_id = uic_id;
        logger.info("UIC_Id was set to {}", uic_id);
    }
}
