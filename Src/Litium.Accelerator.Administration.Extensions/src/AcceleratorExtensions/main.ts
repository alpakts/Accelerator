import { enableProdMode } from "@angular/core";
import { platformBrowserDynamic } from "@angular/platform-browser-dynamic";
import { environment } from "./environments/environment";
import { AcceleratorExtensions } from "./extensions";

if (environment.production) {
    enableProdMode();
}

platformBrowserDynamic()
    .bootstrapModule(AcceleratorExtensions)
    .catch((err) => console.error(err));
