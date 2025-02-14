import { HttpClientModule } from '@angular/common/http';
import { inject, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { PendingChangesGuard, UiModule } from 'litium-ui';
import { FilteringComponent } from './components/filtering/filtering.component';
import { GroupingComponent } from './components/grouping/grouping.component';
import { SearchIndexingComponent } from './components/search-indexing/search-indexing.component';

const appRoutes = [
    {
        path: '',
        children: [
            {
                path: 'filtering',
                component: FilteringComponent,
                canDeactivate: [
                    (component) =>
                        inject(PendingChangesGuard).canDeactivate(component),
                ],
            },
            {
                path: 'grouping',
                component: GroupingComponent,
                canDeactivate: [
                    (component) =>
                        inject(PendingChangesGuard).canDeactivate(component),
                ],
            },
            {
                path: 'searchindexing',
                component: SearchIndexingComponent,
                canDeactivate: [
                    (component) =>
                        inject(PendingChangesGuard).canDeactivate(component),
                ],
            },
        ],
    },
];

@NgModule({
    declarations: [
        FilteringComponent,
        GroupingComponent,
        SearchIndexingComponent,
    ],
    imports: [
        HttpClientModule,
        UiModule,
        TranslateModule,
        RouterModule.forChild(appRoutes),
    ],
})
export class AcceleratorExtensions {}
