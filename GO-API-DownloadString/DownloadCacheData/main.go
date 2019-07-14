package main

import (
	"os"
	"io/ioutil"
	"net/http"
	"time"
)

func main() {
	url := "https://poe.ninja/api/data/itemoverview?league=Legion&type="
	cates := []string{"Currency", "Fragments", "Incubator", "Scarab", "Fossil", "Resonator", "Essences", "DivinationCard", "Prophecy", "SkillGem", "BaseType", "UniqueMap", "Map"}
		WebClient := http.Client {
			Timeout: time.Second *2,
		}
		
		for _, element := range cates {
			e,_ := http.NewRequest(http.MethodGet, url + element, nil)
			
			e.Header.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0")

			ee,_ := WebClient.Do(e)

			downstring,_ := ioutil.ReadAll(ee.Body)
			
			f, _ := os.Create("./cache/" + element + ".cache")
			
			_,_ = f.Write(downstring)
			
			f.Sync()
			f.Close()
		}
}
