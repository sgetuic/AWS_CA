package de.stacksmashers.pushserver.config;

import org.aeonbits.owner.ConfigCache;
import org.aeonbits.owner.ConfigFactory;

import java.io.File;

public class ConfigurationLoader {


    public ConfigurationLoader() {
        String rawPath = new File("config.properties").toURI().getRawPath();
        System.out.println(rawPath);
        ConfigFactory.setProperty("config", rawPath);
    }


    public Configuration loadConfig() {

        //todo propties ladne und reinstecken


        return ConfigCache.getOrCreate(Configuration.class);
    }

}
