import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { environment } from "./environments/environment";
import { Accelerator } from "./extensions";

if (environment.production) {
    enableProdMode();
}

platformBrowserDynamic()
    .bootstrapModule(Accelerator)
    .catch((err) => console.error(err));
